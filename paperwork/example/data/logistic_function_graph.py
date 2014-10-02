import math
import matplotlib.pyplot as plt

def logistic_regression( lower_bounds, upper_bounds ):
    a = 0.0
    b = 0.0

    numerator = (upper_bounds[1] + 0.0001) - (lower_bounds[1] - 0.0001)
    lhs = upper_bounds[1] - lower_bounds[1] + 0.0001

    a =  0.0001 / lhs
    b = math.log( (numerator/0.0001 + 1) / a ) / (lower_bounds[0] - upper_bounds[0] )

    return (a, b)

def logistic_function( original_dist, lower_bounds, upper_bounds ):
    log_vars = logistic_regression( lower_bounds, upper_bounds )
    numerator = (upper_bounds[1] + 0.0001) - (lower_bounds[1] - 0.0001)
    denominator = 1 + log_vars[0] * math.exp( log_vars[1] * (original_dist - upper_bounds[0]))

    return lower_bounds[1] + 0.0001 + numerator / denominator

outer = (200.0, 1.0)
inner = (100.0, 4.0)

x_vals = [i * 1.0 for i in xrange(201)]
y_vals = [logistic_function( x, outer, inner ) for x in x_vals]

plt.plot(x_vals, y_vals)
plt.xlim([100,200])
plt.ylim([0.0,5.0])
plt.xlabel('distance (d)')
plt.ylabel('interpolated zoom level (z\')')
plt.savefig("../img/logistic_graph.png")
plt.show()


