RAY_PATH = "C:\\SpeedyMailer\\Release\\Ray\\SpeedyMailer.Master.Ray.exe"

puts "Load data file"

data_file = ARGV.first

puts "Extract domains"
domain_file = data_file + ".domains.txt"
puts `#{RAY_PATH} -p #{data_file} -o #{domain_file} -x`

puts "Run DNS clean"
bad_domains = data_file + ".bad.domain.txt"
puts `#{RAY_PATH} -p #{domain_file} -o #{bad_domains} -d`

puts "Output clean list"
output_file = data_file + ".clean.txt"
puts `#{RAY_PATH} -p #{domain_file} -o #{output_file} -b #{bad_domains}`