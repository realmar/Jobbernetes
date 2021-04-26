#!/usr/bin/env python3

import shutil
from pathlib import Path
from lib import run_shell_print, SPECS_DIR

if __name__ == "__main__":
    command = "jsonnetfmt --indent 2 --max-blank-lines 2 --sort-imports --string-style s --comment-style s -i {}"

    for file in Path(SPECS_DIR).rglob('*.jsonnet'):
        strfile = str(file.absolute())
        run_shell_print(command.format(strfile))
