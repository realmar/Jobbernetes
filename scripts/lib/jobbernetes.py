#!/usr/bin/env python3

from itertools import chain
from . import kubecfg
from . import get_components


def task(action):
    kubecfg.run(action, (f"kubernetes-specs/{x['name']}.jsonnet" for x in chain(*[v for k, v in get_components().items()])))
