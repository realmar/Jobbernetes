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


def get_volumes():
    jsontext, _ = run_shell(f"jsonnet --ext-str cwd={ROOT_DIR} {K3D_VOLUMES_PATH}")
    return json.loads(jsontext)
