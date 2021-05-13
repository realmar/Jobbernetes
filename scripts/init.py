"""
Add initialization logic run by every script in the delete, deploy, etc.
folders here. For example this is used to to setup the paths.
"""

import sys
from pathlib import Path


def get_absolute(path: Path):
    return str(path.resolve())


path = get_absolute(Path(__file__).parent)
if path not in sys.path:
    sys.path.insert(0, path)
