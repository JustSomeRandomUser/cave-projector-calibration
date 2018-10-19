#@PydevCodeAnalysisIgnore
# pylint: disable-all
# encoding: utf-8
"""
Created on 18.10.2012

@author: Sebastian Brandt

Kompiliert gen_kalib_textures.py als exe.

Usage: setup.py py2exe -c -b 1

Das Ergebnis findet sich im dist-Unterordner
"""

from distutils.core import setup
import py2exe

setup(console=[{"script" : "gen_kalib_textures.py"}])
