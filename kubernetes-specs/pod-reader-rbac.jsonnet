local kube = import 'vendor/kube.libsonnet';

{
  role: kube.Role('pod-reader') {
    rules+: [
      {
        apiGroups: [
          '',
        ],
        resources: [
          'pods',
        ],
        verbs: [
          'get',
          'list',
          'watch',
        ],
      },
    ],
  },
  rolebinding: kube.RoleBinding('default-pod-reader') {
    roleRef_: $.role,
    subjects_: [
      kube.ServiceAccount('default')
      {
        metadata+:
          {
            namespace: 'default',
          },
      },
    ],
  },
}
