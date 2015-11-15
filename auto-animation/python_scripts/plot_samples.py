import numpy as np
import matplotlib as mpl
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import sys
import csv
import math

quiver = False 

if len(sys.argv) < 2:
    sys.exit("Data file required")
if len(sys.argv) >= 3:
    quiver = True

csvfile = open(sys.argv[1], 'rb')
csvreader = csv.reader(csvfile, delimiter=';')

# skip the sep line
next(csvreader)
# skip the title line
next(csvreader)

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

## read from csv file into a list of tuples
## get the energies to [0,1]
data = [(row[0].split(','), row[2].split(','), row[4]) for row in csvreader]

if not quiver:
    print("Energy Plot")
    max_e = float(max(zip(*data)[2]))
    min_e = float(min(zip(*data)[2]))
    diff = max_e - min_e

    for p,a,c in data:
        col = float(1) - ((float(c) - min_e) / diff)
        # need to swap the poitns around a little to match our coordinate system
        # z is vert, we want y as vert
        ax.scatter(float(p[0]), float(p[2]), float(p[1]), c=(float(col), float(col), float(1)), marker='o', s=64)
else:
    print("Torque Plot")
    X, Y, Z = zip(*(zip(*data)[0]))
    U, V, W = zip(*(zip(*data)[1]))

    X = list(map(float, X))
    Y = list(map(float, Y))
    Z = list(map(float, Z))
    U = list(map(float, U))
    V = list(map(float, V))
    W = list(map(float, W))

    magnitudes = [math.sqrt(u * u + v * v + w * w) for u, v, w in zip(U,V,W)]
    min_a = float(min(magnitudes))
    max_a = float(max(magnitudes))
    diff_a = float(max_a - min_a)
    color_mag = [((float(m) - min_a) / diff_a) for m in magnitudes]
    colors = [(abs(u / m * c), abs(v / m * c), abs(w / m * c)) for u, v, w, c, m in zip(U, V, W, color_mag, magnitudes)]
    
    for x,y,z,col in zip(X, Y, Z, colors):
        ax.scatter(x, z, y, c=col, marker='o', s=64)

ax.set_xlabel("X Position")
ax.set_ylabel("Z Position")
ax.set_zlabel("Y Position")

ax.grid(True)
fname = sys.argv[1].split('.')
print(str(fname))
if quiver:
    plt.savefig("." + fname[0] + fname[1] + "_torque.png")
else:
    plt.savefig("." + fname[0] + fname[1] + "_energy.png")
plt.show()
