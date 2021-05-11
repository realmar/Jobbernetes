local jn = import 'jobbernetes.libsonnet';

local jn_full(name) = jn.local_registry(jn.jn_image(name));

local job_image(name) = jn_full('job-' + name);

local Image(name) = {
  relativeName: name,
  imageName: jn.jn_image(name),
  fqdn: jn_full(name),
  registryName: jn.jn_registry_name,
};

local JobImage(name) = Image(name) {
  fqdn: jn_full('job-' + name),
};

{
  ingress: Image('ingress'),
  egress: Image('egress'),
  external_service: Image('external-service'),
  image_scraper_job: JobImage('image-scraper-job'),
}
