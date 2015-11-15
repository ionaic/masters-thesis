import numpy as np
import matplotlib as mpl
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import sys
import csv

if len(sys.argv) < 2:
    sys.exit("Data file required")

csvfile = open(sys.argv[1], 'rb')
csvreader = csv.reader(csvfile, delimiter=';')

# skip the sep line
next(csvreader)
# skip the title line
next(csvreader)

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# read from csv file into a list of tuples
# get the energies to [0,1]
data = [((row[0].split(',')), row[4]) for row in csvreader]

max_e = float(max(zip(*data)[1]))
min_e = float(min(zip(*data)[1]))
diff = max_e - min_e

for p,c in data:
    col = float(1) - ((float(c) - min_e) / diff)
    # need to swap the poitns around a little to match our coordinate system
    # z is vert, we want y as vert
    ax.scatter(float(p[0]), float(p[2]), float(p[1]), c=(float(col), float(col), float(1)), marker='o', s=64)

#X, Y, Z = zip(*(zip(*data)[0]))
#print(str(X))
#
#X = list(map(float, X))
#Y = list(map(float, Y))
#Z = list(map(float, Z))
#
#ax.plot_wireframe(X, Y, Z)

ax.set_xlabel("X Position")
ax.set_ylabel("Z Position")
ax.set_zlabel("Y Position")

ax.grid(True)

#ax.set_xlim(-0.15, 0.15)
#ax.set_ylim(-0.15, 0.15)

plt.savefig(sys.argv[1] + ".png")
plt.show()
