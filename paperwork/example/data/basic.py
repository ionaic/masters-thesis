import matplotlib.pyplot as plt

x_data = [i for i in xrange(320)]
y_data = [1.0 if x < 160 else 6.7 for x in x_data]

plt.plot(x_data, y_data)
plt.title("Simple Linear Magnification")
plt.xlabel("Distance from Focal Point")
plt.ylabel("Zoom Level")
plt.xlim(0, 320)
plt.ylim(0.0, 8.0)
plt.show()
