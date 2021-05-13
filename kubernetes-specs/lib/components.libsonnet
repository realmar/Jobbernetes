local jn = import 'jobbernetes.libsonnet';

{
  services:
    [
      jn.Component('dataviewer'),
      jn.Component('ingress'),
      jn.Component('egress'),
      jn.Component('external-service'),
    ],

  jobs: [
    jn.Component('image-scraper-job'),
  ],
}
