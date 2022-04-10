#!/usr/bin/env python3

destination = open('2022_place_canvas_history_treated.csv', 'w')
destination.write('timestamp,user_hash,x_coordinate,y_coordinate,color\n')

source = open('Resources/2022_place_canvas_history.csv', 'r')
source.readline() # skip header

COLOR_MAP = {
    "#6D001A": 0,  # darkest red
    "#BE0039": 1,  # dark red
    "#FF4500": 2,  # red
    "#FFA800": 3,  # orange
    "#FFD635": 4,  # yellow
    "#FFF8B8": 5,  # pale yellow
    "#00A368": 6,  # dark green
    "#00CC78": 7,  # green
    "#7EED56": 8,  # light green
    "#00756F": 9,  # dark teal
    "#009EAA": 10,  # teal
    "#00CCC0": 11,  # light teal
    "#2450A4": 12,  # dark blue
    "#3690EA": 13,  # blue
    "#51E9F4": 14,  # light blue
    "#493AC1": 15,  # indigo
    "#6A5CFF": 16,  # periwinkle
    "#94B3FF": 17,  # lavender
    "#811E9F": 18,  # dark purple
    "#B44AC0": 19,  # purple
    "#E4ABFF": 20,  # pale purple
    "#DE107F": 21,  # magenta
    "#FF3881": 22,  # pink
    "#FF99AA": 23,  # light pink
    "#6D482F": 24,  # dark brown
    "#9C6926": 25,  # brown
    "#FFB470": 26,  # beige
    "#000000": 27,  # black
    "#515252": 28,  # dark gray
    "#898D90": 29,  # gray
    "#D4D7D9": 30,  # light gray
    "#FFFFFF": 31,  # white
}

count = 0
for line in source:
    count += 1
    line = line.replace('\"', '').strip()
    parts = line.split(',')
    if len(parts) == 5: # classic line
        destination.write(parts[0].replace(" UTC", "") + "," + parts[1] + "," + parts[3] + "," + parts[4] + "," + str(COLOR_MAP[parts[2]]) + '\n')
    else: # moderator line
        print("NOT NORMAL LINE - Line {}: {}".format(count, line.strip()))
    if count % 1000000 == 0:
        print("Line{}: {}".format(count, line.strip()))

source.close()
destination.close()

