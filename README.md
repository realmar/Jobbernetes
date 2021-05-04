# TODO

 - Refactor Egress
 - Refactor hosting so that is more modular
 - Handle consumed data if job throws exception (prob just put it back into the queue --> handling strategies)
 - DI for GRPC services
 - DI modules unification
 - Actual config file and/or environment variables
 - Create and send metrics
 - Create k8s specs and reusable library
 - Testing somehow??
 - Create Grafana dashboards with metrics etc.

Very low prio

 - Nicer DataViewer UI (it's just here to show that it works)
 - Swagger for GRPC services

# BT - PoC

TODO

If you are running on Windows then you NEED WSL2 AND Docker for Windows Desktop
installed. Furthermore, in WSL2 you need to install kubecfg and jsonnet.


https://kubectl.docs.kubernetes.io/installation/kustomize/

https://www.telepresence.io/reference/install

apt update && apt install -y curl

cd /usr/share/nginx/html/

k3d cluster create --servers 3 --volume (pwd)/data:/var/data

Rancher for testing

https://github.com/rabbitmq


Parameterization:
https://kubernetes.io/docs/concepts/configuration/
https://kubernetes.io/docs/concepts/configuration/configmap/

Configuration Best Practises:
https://kubernetes.io/docs/concepts/configuration/overview/

https://github.com/evanlucas/fish-kubectl-completions

https://github.com/kapicorp/kapitan
https://github.com/bitnami/kubecfg
https://github.com/bitnami-labs/kube-libsonnet/blob/master/kube.libsonnet
https://github.com/dhall-lang/dhall-lang
https://github.com/dhall-lang/dhall-kubernetes

https://brew.sh/

## Ingress

http://grafana.localhost:8080/

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
