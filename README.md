# Jobbernetes

This README is a work in progress. Please refer to my thesis for more
information.

Note: developed and tested on WSL2 running Ubuntu 20.04.2 LTS.

**Prerequisites:**

 - [Docker](https://www.docker.com/)
 - [K3d](https://k3d.io/)
 - [Kubectl](https://kubernetes.io/docs/tasks/tools/)
 - [Kufecfg](https://github.com/bitnami/kubecfg)
 - [Jsonnet](https://jsonnet.org/)
 - [Python 3](https://www.python.org/)

**Quick Start:**

```sh
# Create cluster and deploy everything
./scripts/deploy/demo.py

# Start ingesting data into the work queue (using the InputService)
#   Create 2 threads
#   On every thread send a new UUID every 0.06 seconds
./scripts/tools/send_text.py 0.06 -p 2
```

Navigate to http://grafana.localhost/ and open the Jobbernetes dashboard.

## Ingress

 - http://grafana.localhost/
 - http://rabbitmq.localhost/
 - https://rancher.localhost/

Caveats:

Grafana cannot tail logs vom Loki because it tries to connect to a `wss` web
socket. The ingress is not configured to handle this. Firstly, it should not use
encryption, and secondly, I suspect the ingress is not setting the correct
headers.

You need to use port-forwarding to tail the logs:

```sh
./scripts/tools/port_forward.py
```

Access as follows: `http://localhost:9092`

## Commands

```sh
# General
kubectl get po          # po  = pods
kubectl get svc         # svc = services
kubectl get ing         # ing = ingress

# Show more information
kubectl get <type> -o wide

# Show k8s object as yaml (spec + status)
kubectl get <type> <name> -o yaml

# Secrets
kubectl get secrets
kubectl get secret <name> -o jsonpath="{.data}"
kubectl get secret <name> -o jsonpath="{.data.password}" | base64 --decode

# Logs
kubectl logs -f <pod> [<container>]

# Port Forwarding
kubectl port-forward --namespace default svc/<name> <host>:<k8s>

# Storage
kubectl get sc      # sc = StorageClass
kubectl get pv      # pv = PersistentVolume
kubectl get pvc     # sc = PersistentVolumeClaim
```

Switch context

Also this: https://github.com/ahmetb/kubectx

```sh
# https://k3d.io/usage/kubeconfig/
set -gx KUBECONFIG (k3d kubeconfig write jobbernetes-cluster)
```

Manually trigger CronJobs:

https://stackoverflow.com/questions/40401795/how-can-i-trigger-a-kubernetes-scheduled-job-manually

```sh
kubectl create job --from=cronjob/<cronjob-name> <job-name>
```

## sysctl

```sh
sudo sysctl -w sysctl vm.swappiness=2
sudo sysctl -w fs.inotify.max_user_instances=512
```

Show swap usage per process

```sh
for file in /proc/*/status ; awk '/VmSwap|Name/{printf $2 " " $3}END{ print ""}' $file; end | sort -k 2 -n -r
```

## Dependencies

Python

```sh
pip install requests
```
