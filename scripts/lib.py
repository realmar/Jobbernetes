import subprocess
import pathlib
import json
import os


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


ROOT_DIR = str(pathlib.Path(__file__).parents[1].absolute())

K3D_VOLUMES_FILE = "k3d.volumes.jsonnet"
K3D_VOLUMES_PATH = os.path.join(ROOT_DIR, K3D_VOLUMES_FILE)

K3D_CONFIG_FILE = "k3d.config.yaml"
K3D_CONFIG_PATH = os.path.join(ROOT_DIR, K3D_CONFIG_FILE)

K3D_CLUSTER_NAME = "jobbernetes-cluster"

SPECS_DIR = os.path.join(ROOT_DIR, "kubernetes-specs")


def is_windows():
    return os.name == 'nt'


def run_shell(command: str, silent=False):
    if not silent:
        print(command)

    result = subprocess.run(command.split(" "),
                            stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE)

    error = ""

    if result.stderr:
        error = result.stderr.decode("utf-8")

    if result.returncode != 0:

        print()
        print(Colors.FAIL + "ERROR: " + Colors.ENDC + error)
        print()

        raise Exception(error)

    return (result.stdout.decode("utf-8"), error)


def run_shell_print(command: str):
    stdout, stderr = run_shell(command)

    if stdout:
        print(stdout)

    if stderr:
        print(stderr)

    return (stdout, stderr)


def __read_jsonnet(path):
    escaped_root_dir = ROOT_DIR

    if is_windows():
        escaped_root_dir = escaped_root_dir.replace('\\', '\\\\')

    command = f"jsonnet --ext-str cwd={escaped_root_dir}"
    if is_windows():
        wsl_root, _ = run_shell(f"wsl -- pwd")
        wsl_root = wsl_root.strip()
        wsl_path = f"{wsl_root}/{path}"

        print(wsl_path)
        command = f"wsl -- bash -l -c '{command} {wsl_path}'"
        print(command)
    else:
        command = f"{command} {path}"

    jsontext, _ = run_shell(command)

    return json.loads(jsontext)


def get_volumes():
    return __read_jsonnet(K3D_VOLUMES_FILE)


def get_container_images():
    return __read_jsonnet(os.path.join(SPECS_DIR, 'lib', 'container_images.libsonnet'))
