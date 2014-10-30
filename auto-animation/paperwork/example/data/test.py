import math

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

def inverse_logistic_function( original_dist, lower_bounds, upper_bounds ):
    log_vars = logistic_regression( lower_bounds, upper_bounds )
    s2 = upper_bounds[1] - lower_bounds[1] + 2 * 0.0001
    s1 = 1 + 0.0001

    if( s2 /(original_dist -s1) < 1 ):
        print "ugh"
        return upper_bounds[0]

    return (math.log( (s2/(original_dist-s1) - 1) ) - math.log( log_vars[0] )) / log_vars[1] + upper_bounds[0]

zoom_levels = [64.0]
z_levels = [ i * 0.05 for i in xrange (81) ]
distances = [ i * 5.0 for i in xrange(41) ]

headers = ["Type", "Original Dist", "ZOOM",  "New Dist", "Status", "", "Type", "Original Dist", "ZOOM", "New Dist", "Status"]
seperator = "-" * 15 * len(headers);

row_format = "{:>15}" * len(headers)

for dist in distances:
    outer = (200.0, 1.0)
    inner = (100.0, 4.0)

    mag_level = logistic_function( dist, outer, inner )
    print "DIST: %f MAG: %f" % (dist, mag_level)

print
for z in z_levels:
    outer = (200.0, 1.0)
    inner = (100.0, 4.0)

    if z >= 1.0 :
        dist = inverse_logistic_function( z, outer, inner )
        print "MAG %f DIST: %f" % (z, dist)

uv_data = []

zoom_levels = [4.0]
distances = [ i * 5.0 for i in xrange(81) ]
print distances

for zoom_level in zoom_levels:
    outer = (400.0, 1.0)
    inner = (100.0, zoom_level)

    for dist in distances:
        if dist <= inner[0]:
            mag_type = "LINEAR"
            mag_level = 1.0 / zoom_level
            new_dist = dist * mag_level
            status = "GOOD"

            mag_level = "{0:.5f}".format(mag_level)
            new_dist = "{0:.5f}".format(new_dist)

            row = [mag_type, dist, mag_level, new_dist, status]
            uv_data.append(row)

        elif dist <= outer[0]:
            mag_type = "NON-LINEAR"
            mag_level = logistic_function( dist, outer, inner )

            mag_level = 1.0 / mag_level

            power_exp = math.exp(10)
            new_dist = power_exp / ( power_exp - 1.0 ) * mag_level * dist

            mag_level = "{0:.5f}".format(mag_level)
            new_dist = "{:.5f}".format(new_dist)
            status = "GOOD"

            row = [mag_type, dist, mag_level, new_dist, status]
            uv_data.append(row)

node_data = []

for zoom_level in zoom_levels:
    outer = (400.0, 1.0)
    inner = (100.0, zoom_level)

    total = 0
    errors = 0

    for i, dist in enumerate(distances):
        mag_type = "LINEAR"
        if( dist <= inner[0]/zoom_level ):
            mag_level = zoom_level;
            new_dist = dist *  mag_level
            new_dist = "{:.5f}".format(new_dist)
            mag_level = "{0:.5f}".format(mag_level)
            status = "GOOD"

            row = [mag_type, dist, mag_level, new_dist, status]
            node_data.append(row)

        elif( dist <= outer[0] ):
            mag_type = "NON-LINEAR"

            m = (outer[1] - inner[1]) / (outer[0] - inner[0])
            b = outer[1] - m * outer[0]

            mag = dist * m + b
            #print "mag: %f, dist: %f" % (mag, dist)
            mag_level = inverse_logistic_function( mag, outer, inner )

            new_dist = mag_level
            #print "new_dist: %f" % new_dist

            mag_level = new_dist / dist

            status = "GOOD"
            if new_dist > 200 :
                status = "BAD"

            new_dist = "{:.5f}".format(new_dist)
            mag_level = "{0:.5f}".format(mag_level)


            row = [mag_type, dist, mag_level, new_dist, status]
            node_data.append(row)

data_sep = ["|"]
merged_data = [ uv_data[i] + data_sep + node_data[i] for i in xrange(len(node_data)) ]

print row_format.format( *headers );
print seperator

for row in merged_data:
    print row_format.format(*row)
