local comp = import 'components.libsonnet';
local jn = import 'jobbernetes.libsonnet';

{
  [std.strReplace(x.name, '-', '_')]: jn.Image(x.name)
  for x in comp.services
}
+
{
  [std.strReplace(x.name, '-', '_')]: jn.JobImage(x.name)
  for x in comp.jobs
}
