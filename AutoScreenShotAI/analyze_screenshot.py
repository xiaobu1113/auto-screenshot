#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AutoScreenShotAI - 截图 AI 分析脚本
使用 LangChain + 智谱 GLM 视觉模型分析截图，返回纯文本结果。

用法:
    python analyze_screenshot.py <image_path> [--prompt "提示词"] [--model ...]

环境变量:
    ZAI_API_KEY    智谱 API 密钥（必须）
    ZAI_BASE_URL   默认 https://open.bigmodel.cn/api/paas/v4/
    VISION_MODEL     默认 glm-4.6v-flash
"""

import argparse
import base64
import logging
import os
import sys
import time
from typing import Optional

# ==================== 配置日志 ====================
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    stream=sys.stderr,          # 全部日志输出到 stderr，stdout 只放结果
)
logger = logging.getLogger(__name__)

# ==================== 默认值 ====================
DEFAULT_API_BASE = "https://open.bigmodel.cn/api/paas/v4/"
DEFAULT_VISION_MODEL = "glm-4.6v-flash"
DEFAULT_PROMPT = (
    "请详细描述这张截图的内容，包括所有可见的文字、UI元素、布局和关键信息。"
)
MAX_IMAGE_BYTES = 20 * 1024 * 1024   # 20MB，超过会警告
MAX_RETRIES = 2                      # 失败重试次数
RETRY_DELAY = 2                      # 重试间隔秒数

# ==================== 强制 UTF-8 ====================
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")


def load_api_key() -> str:
    """从环境变量读取 API Key，没有则 panic"""
    key = os.environ.get("ZAI_API_KEY")
    if not key:
        logger.error("环境变量 ZAI_API_KEY 未设置")
        sys.exit(1)
    return key


def image_to_base64(image_path: str) -> Optional[str]:
    """将图片编码为 base64，超过限制则报警"""
    file_size = os.path.getsize(image_path)
    if file_size > MAX_IMAGE_BYTES:
        logger.warning(
            f"图片过大 ({file_size / 1024 / 1024:.1f} MB)，可能超过模型限制，分析可能失败"
        )

    try:
        with open(image_path, "rb") as f:
            return base64.b64encode(f.read()).decode("utf-8")
    except Exception as e:
        logger.error(f"无法读取图片: {e}")
        return None


def build_image_data_url(image_path: str) -> Optional[str]:
    """返回图片的 data URL，扩展名自适应"""
    ext = os.path.splitext(image_path)[1].lower().lstrip(".")
    # 智谱支持的格式
    supported = {"png", "jpg", "jpeg", "gif", "webp", "bmp"}
    if ext not in supported:
        logger.warning(f"不常见的图片格式 .{ext}，视为 png")
        ext = "png"
    b64 = image_to_base64(image_path)
    if b64 is None:
        return None
    return f"data:image/{ext};base64,{b64}"


def analyze_image(image_path: str, prompt: str, api_key: str, base_url: str, model: str) -> str:
    """
    调用智谱 GLM 视觉模型分析图片，返回模型输出文本。
    出现可恢复错误时会自动重试。
    """
    # 先尝试导入（避免全局 import 影响启动速度）
    try:
        from langchain_openai import ChatOpenAI
        from langchain_core.messages import HumanMessage
    except ImportError as e:
        logger.error(f"缺少依赖库: {e}，请执行 pip install langchain-openai langchain-core")
        sys.exit(1)

    image_url = build_image_data_url(image_path)
    if image_url is None:
        sys.exit(1)

    last_exception = None
    for attempt in range(MAX_RETRIES + 1):
        try:
            llm = ChatOpenAI(
                api_key=api_key,
                base_url=base_url,
                model=model,
                max_tokens=4096,
                request_timeout=30,          # 单次请求超时 30 秒
            )

            message = HumanMessage(
                content=[
                    {"type": "text", "text": prompt},
                    {"type": "image_url", "image_url": {"url": image_url}},
                ]
            )

            response = llm.invoke([message])
            content = response.content if hasattr(response, "content") else str(response)
            return content

        except Exception as e:
            last_exception = e
            logger.warning(f"第 {attempt + 1} 次调用失败: {e}")
            if attempt < MAX_RETRIES:
                logger.info(f"等待 {RETRY_DELAY} 秒后重试...")
                time.sleep(RETRY_DELAY)

    # 所有重试均失败
    logger.error(f"分析失败，已重试 {MAX_RETRIES} 次，最后错误: {last_exception}")
    sys.exit(1)


def main():
    parser = argparse.ArgumentParser(description="使用智谱 GLM 视觉模型分析截图")
    parser.add_argument("image_path", help="截图文件的路径")
    parser.add_argument("--prompt", help="分析提示词（可选）")
    parser.add_argument("--model", default=os.environ.get("VISION_MODEL", DEFAULT_VISION_MODEL),
                        help=f"模型名称（默认 {DEFAULT_VISION_MODEL}）")
    parser.add_argument("--base-url", default=os.environ.get("ZHIPU_BASE_URL", DEFAULT_API_BASE),
                        help="智谱 API 基地址")
    args = parser.parse_args()

    image_path = args.image_path
    if not os.path.isfile(image_path):
        logger.error(f"图片文件不存在: {image_path}")
        sys.exit(1)

    # 提示词优先级：命令行 > 环境变量 > 默认
    prompt = args.prompt or os.environ.get("SCREENSHOT_AI_PROMPT", DEFAULT_PROMPT)
    # 还原可能被 shell 转义的引号（C# 调用时会做处理，这里保留）
    prompt = prompt.replace('\\"', '"')

    api_key = load_api_key()
    base_url = args.base_url.rstrip("/") + "/"

    # 执行分析
    result = analyze_image(image_path, prompt, api_key, base_url, args.model)

    # 只将结果输出到 stdout，C# 会读取它
    print(result)


if __name__ == "__main__":
    main()