import runpy
import pathlib

runpy.run_path(str(pathlib.Path(__file__).parent.parent.joinpath('init.py')), run_name='__main__')

