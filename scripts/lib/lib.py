import importlib
import subprocess
import pathlib
import json
import os
import sys

# region Constants


class Colors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'


ROOT_DIR = str(pathlib.Path(__file__).parents[2].absolute())

K3D_VOLUMES_FILE = "k3d.volumes.jsonnet"
K3D_VOLUMES_PATH = os.path.join(ROOT_DIR, K3D_VOLUMES_FILE)

K3D_CONFIG_FILE = "k3d.config.yaml"
K3D_CONFIG_PATH = os.path.join(ROOT_DIR, K3D_CONFIG_FILE)

K3D_CLUSTER_NAME = "jobbernetes-cluster"

SPECS_DIR = os.path.join(ROOT_DIR, "kubernetes-specs")

# endregion

# region Private API


def __is_windows():
    return os.name == 'nt'


def __run_shell(command: str, silent=False, pipe=False):
    if not silent:
        print(command)

    args = {}
    if not pipe:
        args = {
            'stdout': subprocess.PIPE,
            'stderr': subprocess.PIPE
        }

    if '|' in command:
        result = subprocess.run(command, shell=True, **args)
    else:
        result = subprocess.run(command.split(" "), **args)

    error = ""

    if result.stderr:
        if pipe:
            error = str(result.returncode)
        else:
            error = result.stderr.decode("utf-8")

    if result.returncode != 0:

        print()
        print(Colors.FAIL + "ERROR: " + Colors.ENDC + error)
        print()

        raise Exception(error)

    return (result.stdout.decode("utf-8") if not pipe else "", error)


def __read_jsonnet(path):
    escaped_root_dir = ROOT_DIR

    if __is_windows():
        escaped_root_dir = escaped_root_dir.replace('\\', '\\\\')

    command = f"jsonnet --ext-str cwd={escaped_root_dir}"
    if __is_windows():
        wsl_root, _ = __run_shell(f"wsl -- pwd")
        wsl_root = wsl_root.strip()
        wsl_path = f"{wsl_root}/{path}"

        print(wsl_path)
        command = f"wsl -- bash -l -c '{command} {wsl_path}'"
        print(command)
    else:
        command = f"{command} {path}"

    jsontext, _ = __run_shell(command)

    return json.loads(jsontext)


def __load_module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)

    return module

# endregion

# region Public API


def is_windows():
    return __is_windows()


def run_shell(command: str, silent=False):
    return __run_shell(command, silent)


def run_shell_print(command: str):
    __run_shell(command, pipe=True)


def get_volumes():
    return __read_jsonnet(K3D_VOLUMES_FILE)


def get_container_images():
    components = __read_jsonnet(os.path.join(SPECS_DIR, 'lib', 'components.libsonnet'))
    return [v['image'] for k, v in components.items()]


def get_registry():
    return __read_jsonnet(os.path.join(SPECS_DIR, 'lib', 'registry.libsonnet'))


def get_components():
    return __read_jsonnet(os.path.join(SPECS_DIR, 'lib', 'components.libsonnet'))


def does_registry_exists(name):
    result, _ = run_shell("k3d registry list", silent=True)
    return name in result


def inject_private_secrets():
    path = os.path.join(ROOT_DIR, 'scripts', 'lib', 'private_secrets.py')

    if os.path.exists(path):
        print()
        print('Injecting private secrets...')
        print()

        private_secrets = __load_module("private_secrets", path)
        private_secrets.inject()


def print_big_header(message):
    print()
    print('################################################################')
    print('## \u001b[1m' + message + '\u001b[0m')
    print('################################################################')
    print()


def print_medium_header(message):
    print()
    print('## \u001b[1m' + message + '\u001b[0m')
    print()

# endregion
