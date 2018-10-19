# -*- coding: Cp1252 -*-
""""
Created on 28.08.2013

@author: Sebastian Brandt

Kleines Skript, das alle für die Beamerkalibrierung benötigten Texturen
AUßER für den Speichermodus erzeugt.
"""
from __future__ import division
from errno import EEXIST
from os import makedirs, chdir
from PIL import Image, ImageDraw, ImageFont

# Maximale Ausdehung des Beamers in Pixeln. Das Programm geht zur Zeit davon
# aus, dass damit die Breite gemeint ist.
MAX_SCREEN_PX = 1024
# Die Leinwandgröße in Meter. Wird für das Seitenverhältnis verwendet.
# Der Boden ist 3x3, die Seiten 2x2.3. Wir nehmen das größere...
SCREEN_SIZE = (3, 3)
# Wie viele Kacheln soll es in jede Richtung geben?
# D.h., es gibt TILES-1 Farbwechsel in x- und y-Richtung
TILES = 9

# Farben der Kacheln im aktiven Zustand (grün/türkis)
TILE_COLORS = ((153, 204, 255), (51, 153, 0))
# Farben der Kacheln im inaktiven Zustand (hellgrau/dunkelgrau)
TILE_COLORS_INACTIVE = ((204, 204, 204), (102, 102, 102))
# Farbe der Ecken (rot)
CORNER_COLOR = (204, 51, 0)
# Die Ecken werden Hühe * Faktor groß sein
CORNER_SIZE_FACTOR_BIG = 0.15
CORNER_SIZE_FACTOR_SMALL = 0.1
# Diese Schriftart wird für die Zahlen (Beamernummer) verwendet
FONT_FILE = 'C:\Windows\Fonts\calibrib.ttf'
# In diesem Format wird gespeichert. Gleichzeitig die Dateiendung
FORMAT = 'png'
# In diesen Unterordner des aktuellen Arbeitsverzeichnisses werden die
# Texturen gespeichert.
OUT = 'data'


# ENDE EINSTELLUNGEN

# Umkehrungen...
TILE_COLORS_REV = tuple(reversed(TILE_COLORS))
TILE_COLORS_INACTIVE_REV = tuple(reversed(TILE_COLORS_INACTIVE))

SCREEN_RATIO = SCREEN_SIZE[1] / SCREEN_SIZE[0]

# ANNAHME: Bildschirm immer breiter als hoch.
TEX_SIZE = (MAX_SCREEN_PX, int(MAX_SCREEN_PX * SCREEN_RATIO))

# Zentrum der Textur
TEX_CENTER = (TEX_SIZE[0] // 2, TEX_SIZE[1] // 2)

RADIUS = TEX_SIZE[0] / 6
CIRCLE_ARG = (TEX_CENTER[0] - RADIUS, TEX_CENTER[1] - RADIUS,
              TEX_CENTER[0] + RADIUS, TEX_CENTER[1] + RADIUS)

# Wenn TRUE, wird TILE_COLORS_*_REV genommen, sonst ohne REV
FLIP = [False, False, True, True, False, False, False, False]

# Größe der Ecken in Pixel
CS_BIG = TEX_SIZE[1] * CORNER_SIZE_FACTOR_BIG
CS_SMALL = TEX_SIZE[1] * CORNER_SIZE_FACTOR_SMALL

# Polygon-Pfade der Ecken
CORNER_COORDS_BIG = (
    [(0, 0), (0, CS_BIG), (CS_BIG, 0)],
    [(TEX_SIZE[0], 0), (TEX_SIZE[0], CS_BIG), (TEX_SIZE[0] - CS_BIG, 0)],
    [(TEX_SIZE[0], TEX_SIZE[1]), (TEX_SIZE[0], TEX_SIZE[1] - CS_BIG),
     (TEX_SIZE[0] - CS_BIG, TEX_SIZE[1])],
    [(0, TEX_SIZE[1]), (0, TEX_SIZE[1] - CS_BIG), (CS_BIG, TEX_SIZE[1])],
)
CORNER_COORDS_SMALL = (
    [(0, 0), (0, CS_SMALL), (CS_SMALL, 0)],
    [(TEX_SIZE[0], 0), (TEX_SIZE[0], CS_SMALL), (TEX_SIZE[0] - CS_SMALL, 0)],
    [(TEX_SIZE[0], TEX_SIZE[1]), (TEX_SIZE[0], TEX_SIZE[1] - CS_SMALL),
     (TEX_SIZE[0] - CS_SMALL, TEX_SIZE[1])],
    [(0, TEX_SIZE[1]), (0, TEX_SIZE[1] - CS_SMALL), (CS_SMALL, TEX_SIZE[1])],
)

WALL_DECIDER = lambda px, py, fx, fy: (px // fx) % 2 == (py // fy) % 2
GROUND_DECIDER = lambda px, py, fx, fy: (
    (px // fx % 2 == py // fy % 2) !=
    ((px / (py + 1) < 1) != ((TEX_SIZE[0] - px) / (py + 1) < 1))
)


def mkdir_p(path):
    """ Python-Version von 'mkdir -p': Rekursiv erstellen """
    try:
        makedirs(path)
    except OSError as exc:
        if exc.errno == EEXIST:
            pass
        else:
            raise


def gen_base_texture(cols, decider=1):
    """
    Kacheltextur generieren. cols ist ein objekt mit zwei Elementen, die
    Farben enthalten.
    """
    tex = Image.new("RGB", TEX_SIZE, (255, 255, 255))
    pix = tex.load()

    # Zur Berechnung der Farbe einfach die gesamte Pixelgröße auf Anzahl der
    # Tiles verkleinern und dann sehen, was Modulo 2 gibt
    fx = (TEX_SIZE[0] / TILES)
    fy = (TEX_SIZE[1] / TILES)
    gen = ((px, py) for px in range(TEX_SIZE[0]) for py in range(TEX_SIZE[1]))

    for px, py in gen:
        pix[px, py] = cols[0] if decider(px, py, fx, fy) else cols[1]

    return tex


def draw_id(tex, screen_id):
    """ Kreis mit screen_id in die Mitte der Textur malen """
    text = str(screen_id)
    draw = ImageDraw.Draw(tex)
    draw.ellipse(CIRCLE_ARG, fill=(51, 153, 255))

    font = ImageFont.truetype(FONT_FILE, TEX_SIZE[1] // 3)

    textwidth, textheight = draw.textsize(text, font=font)
    draw.text(((TEX_SIZE[0] - textwidth) / 2, (TEX_SIZE[1] - textheight) / 2),
              text, (255, 255, 255), font=font)


def gen_marks(tex, screen_id):
    """
    In die Textur jeweils große und kleine Ecken in jeder Ecke erzeugen.
    Macht also 8 Texturen.
    """
    for i in range(4):
        tmp = tex.copy()
        draw = ImageDraw.Draw(tmp)
        draw.polygon(CORNER_COORDS_BIG[i], fill=CORNER_COLOR)
        tmp.save('cave%02dg%d.%s' % (screen_id, i + 1, FORMAT))

    for i in range(4):
        tmp = tex.copy()
        draw = ImageDraw.Draw(tmp)
        draw.polygon(CORNER_COORDS_SMALL[i], fill=CORNER_COLOR)
        tmp.save('cave%02dk%d.%s' % (screen_id, i + 1, FORMAT))


def gen_textures(screen_id):
    """
    Alle Texturen für eine screen_id (1..8) erzeugen. Insgesamt 9 Stück:
    8 * Ecken, 1 * ohne Ecken
    """
    print 'Generiere Texturen für Beamer', screen_id
    tex = gen_base_texture(
        TILE_COLORS_REV if FLIP[screen_id - 1] else TILE_COLORS,
        WALL_DECIDER if screen_id < 7 else GROUND_DECIDER)
    draw_id(tex, screen_id)
    tex.save('cave%02d.%s' % (screen_id, FORMAT))

    gen_marks(tex, screen_id)


def gen_inactive_textures():
    """
    Generiert die inaktiven Texturen. Nur Kacheln aus TILE_COLORS_INACTIVE,
    cave_a und interviertes cave_b
    """
    print 'Generiere inaktive Texturen...'
    gen_base_texture(TILE_COLORS_INACTIVE,
                     WALL_DECIDER).save('cave_a.%s' % FORMAT)

    gen_base_texture(TILE_COLORS_INACTIVE_REV,
                     WALL_DECIDER).save('cave_b.%s' % FORMAT)

    gen_base_texture(TILE_COLORS_INACTIVE,
                     GROUND_DECIDER).save('cave_c.%s' % FORMAT)


def gen_all_textures():
    """ Generiert alle Texturen """
    gen_inactive_textures()
    for i in range(1, 9):
        gen_textures(i)

if __name__ == '__main__':
    mkdir_p(OUT)
    chdir(OUT)
    gen_all_textures()
    print 'Fertig.'
