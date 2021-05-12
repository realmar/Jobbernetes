{
  registryPort: '5050',
  registryDns: 'registry.localhost',
  registryUrl: 'k3d-' + self.registryUrlRelative,
  registryUrlRelative: self.registryDns + ':' + self.registryPort,
}
