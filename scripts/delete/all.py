#!/usr/bin/env python3

import __init__
import delete.helm as helm
import delete.data as data
import delete.jobbernetes as jobbernetes
import delete.infrastructure as infrastructure


def delete():
    jobbernetes.delete()
    helm.delete()
    infrastructure.delete()
    data.delete(shallow=True)


if __name__ == "__main__":
    delete()
