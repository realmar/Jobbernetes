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
https://dev.to/drcloudycoder/kubernetes-cronjob-best-practices-4nlk

https://github.com/evanlucas/fish-kubectl-completions

https://github.com/kapicorp/kapitan
https://github.com/bitnami/kubecfg
https://github.com/bitnami-labs/kube-libsonnet/blob/master/kube.libsonnet
https://github.com/dhall-lang/dhall-lang
https://github.com/dhall-lang/dhall-kubernetes

https://brew.sh/

Instance Name
https://kubernetes.io/docs/tasks/inject-data-application/environment-variable-expose-pod-information/
https://github.com/prometheus/prometheus/wiki/Default-port-allocations


https://www.rabbitmq.com/semantics.html
https://www.rabbitmq.com/confirms.html
https://www.rabbitmq.com/tutorials/amqp-concepts.html
https://www.rabbitmq.com/queues.html
https://www.rabbitmq.com/amqp-0-9-1-reference.html
https://www.rabbitmq.com/management.html

## Docker Rate Limiting

https://www.docker.com/increase-rate-limits?utm_source=docker&utm_medium=web%20referral&utm_campaign=increase%20rate%20limit&utm_budget=
https://www.docker.com/pricing

> On November 20, 2020, rate limits anonymous and free authenticated use of Docker Hub went into effect. > Anonymous and Free Docker Hub users are limited to 100 and 200 container image pull requests per six > hours. You can read here for more detailed information.
> 
> If you are affected by these changes you will receive this error message:
> 
> ERROR: toomanyrequests: Too Many Requests.
> 
> OR
> You have reached your pull rate limit. You may increase the limit by authenticating and upgrading: > https://www.docker.com/increase-rate-limits. You must authenticate your pull requests.
> 
> To increase your pull rate limits you can upgrade your account to a Docker Pro or Team subscription.
> 
> The rate limits of 100 container image requests per six hours for anonymous usage, and 200 container > image requests per six hours for free Docker accounts are now in effect. Image requests exceeding these > limits will be denied until the six hour window elapses.
> 
> NOTE: Docker Pro and Docker Team accounts enable 50,000 pulls in a 24 hour period from Docker Hub.  

## Building Images

https://hub.docker.com/_/microsoft-dotnet-runtime/
https://hub.docker.com/_/microsoft-dotnet-aspnet

https://andrewlock.net/optimising-asp-net-core-apps-in-docker-avoiding-manually-copying-csproj-files-part-2/
https://www.softwaredeveloper.blog/multi-project-dotnet-core-solution-in-docker-image

https://github.com/genuinetools/reg
https://github.com/andrey-pohilko/registry-cli

## Ingress

http://grafana.localhost/
http://rabbitmq.localhost/

Caveats:

Grafana cannot tail logs vom Loki because it tries to connect to a `wss` web
socket. The ingress is not configured to handle this. Firstly, it should not use
encryption, and secondly, I suspect the ingress is not setting the correct
headers.

You need to use port-forwarding to tail the logs:

```sh
kubectl port-forward --namespace default svc/grafana 9092:80
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
