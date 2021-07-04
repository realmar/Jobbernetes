local Registry = import '../lib/registry.libsonnet';

local image_name(name) = 'jn-' + name;

local image_fqn(name) = Registry.registryUrl + '/' + image_name(name);

local Image(name) = Registry {
  relativeName: name,
  imageName: image_name(name),
  fqn: image_fqn(name),
};

local JobImage(name) = Image(name) {
  fqn: image_fqn('job-' + name),
};

local Component(_name, _ingress=null) = {
  name: _name,
  serviceName: image_name(_name),
  ingress: if std.isString(_ingress) then _ingress,
};

local collection = {
  services:
    [
      Component('dataviewer', 'dataviewer.jn.realmar.net'),
      Component('adminweb', 'admin.jn.realmar.net'),
      Component('input-service', 'input-service.jn.realmar.net'),
      Component('output-service'),
      Component('external-service', 'external-service.jn.realmar.net'),
    ],

  jobs: [
    Component('image-scrape-job'),
  ],
};

{
  [std.strReplace(x.name, '-', '_')]: x {
    image: Image(x.name),
  }
  for x in collection.services
}
+
{
  [std.strReplace(x.name, '-', '_')]: x {
    image: JobImage(x.name),
  }
  for x in collection.jobs
}
