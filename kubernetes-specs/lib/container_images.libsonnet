local jn = import 'jobbernetes.libsonnet';

{
  ingress: jn.Image('ingress'),
  egress: jn.Image('egress'),
  external_service: jn.Image('external-service'),
  image_scraper_job: jn.JobImage('image-scraper-job'),
}
