#!/usr/bin/env python3

import __init__
from itertools import chain
from pathlib import Path
from lib import run_shell_print, SPECS_DIR, K3D_VOLUMES_PATH

if __name__ == "__main__":
    command = "jsonnetfmt --indent 2 --max-blank-lines 2 --sort-imports --string-style s --comment-style s -i {}"

    for file in chain(Path(SPECS_DIR).rglob('*.jsonnet'), Path(SPECS_DIR).rglob('*.libsonnet'), [Path(K3D_VOLUMES_PATH)]):
        strfile = str(file.absolute())

        if "vendor" not in strfile:
            run_shell_print(command.format(strfile))
