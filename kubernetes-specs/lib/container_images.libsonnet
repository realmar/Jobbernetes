local jn = import 'jobbernetes.libsonnet';

local jn_full(name) = jn.local_registry(jn.jn_image(name));

local job_image(name) = jn_full('job-' + name);


{
  ingress: jn_full('ingress'),
  egress: jn_full('egress'),
  external_service: jn_full('external-service'),
  image_scraper_job: job_image('image-scraper'),
}
