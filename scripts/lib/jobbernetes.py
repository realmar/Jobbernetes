#!/usr/bin/env python3

from . import kubecfg
from . import get_components


def task(action):
    kubecfg.run(action, (f"kubernetes-specs/{v['name']}.jsonnet" for k, v in get_components().items()))
