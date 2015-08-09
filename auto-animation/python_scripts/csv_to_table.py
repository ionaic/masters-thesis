import csv, sys

full_table = False

if len(sys.argv) < 2:
    print("CSV input filename required")
    sys.exit()
if len(sys.argv) < 3:
    print("LaTeX output filename required")
    sys.exit()
if len(sys.argv) > 3:
    full_table = True

csvfile = open(sys.argv[1], 'rb')

# dance around to get the number of fields
csvreader = csv.reader(csvfile, delimiter=';')
fieldnum = len(next(csvreader))
csvfile.seek(0);

if full_table:
    outstring = '\\begin{table}[ht]\n\t\\begin{tabular}{|' +  (' c |' * fieldnum) + '}\n'
    outstring += ' \\\\ \\hline\n'.join(['\t\t' + ' & '.join(row) for row in csvreader])
    outstring += '\n\t\\end{tabular}\n\\end{table}'
else:
    outstring = '\\begin{tabular}{|' +  (' c |' * fieldnum) + '}\n'
    outstring += ' \\\\ \\hline\n'.join(['\t' + ' & '.join(row) for row in csvreader])
    outstring += '\n\\end{tabular}'

outfile = open(sys.argv[2], 'wb')
outfile.write(outstring)
outfile.close()

csvfile.close()
