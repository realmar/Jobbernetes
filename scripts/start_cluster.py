#!/usr/bin/env python3

import shutil
import os
import pathlib
import json
import subprocess

ROOT_DIR = str(pathlib.Path(__file__).parents[1].absolute())

K3D_CLUSTER_NAME = "djs-cluster"

K3D_VOLUMES_FILE = "k3d.volumes.json"
K3D_VOLUMES_PATH = os.path.join(ROOT_DIR, K3D_VOLUMES_FILE)

K3D_CONFIG_FILE = "k3d.config.yaml"
K3D_CONFIG_PATH = os.path.join(ROOT_DIR, K3D_CONFIG_FILE)

required_commands = ["k3d", "kubectl", "docker"]


def run_shell(command: str):
    result = subprocess.run(command.split(" "),
                            stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE)

    if result.returncode != 0:
        raise Exception(result.stderr.decode("utf-8"))

    return result.stdout.decode("utf-8")


def format_placeholders(s: str):
    return s.replace("<ROOT>", ROOT_DIR)


def get_volumes():
    with open(K3D_VOLUMES_PATH) as f:
        contents = format_placeholders(f.read())
        return json.loads(contents)


def create_volumes(volumes):
    for volume in volumes:
        host_path = volume["host"]
        if not os.path.exists(host_path):
            print(f"Volume {host_path} does not exist. Creating ...")
            os.mkdir(host_path)
        else:
            print(f"Volume {host_path} already exist. Nothing to do.")


def create_k3d_cli_volumes(volumes):
    return ' '.join(f"--volume {v['host']}:{v['node']}" for v in volumes)


def does_cluster_exist():
    result = run_shell("k3d cluster list")
    return K3D_CLUSTER_NAME in result


if __name__ == "__main__":
    missing = [x for x in required_commands if not shutil.which(x)]
    if any(missing):
        print("Following tools are missing, install them accoring to the readme")
        for tool in missing:
            print(f"  {tool}")

    volumes = get_volumes()
    create_volumes(volumes)

    if does_cluster_exist():
        print(
            "Cluster found. "
            + f"Execute 'k3d cluster delete {K3D_CLUSTER_NAME}' do delete the cluster. "
            + "Starting ...")
        command = f"k3d cluster start {K3D_CLUSTER_NAME}"
    else:
        print(f"Cluster not found. Creating ...")
        command = f"k3d cluster create {K3D_CLUSTER_NAME} --config {K3D_CONFIG_PATH} {create_k3d_cli_volumes(volumes)}"

    print(command)
    run_shell(command)
