require 'albacore'

namespace :windows do

  SOLUTION_FILE = "SpeedyMailer.Drones\\SpeedyMailer.Drones.csproj"
  OUTPUT_FOLDER = "..\\Out\\Drone"

  desc "clean the solution"
  msbuild :clean do |msb|
    msb.targets :Clean
    msb.solution  = SOLUTION_FILE
  end

  desc "Build the solution"
  msbuild :build => :clean do |msb|
    msb.properties :configurations => :Release,:OutputPath => OUTPUT_FOLDER
    msb.targets :Rebuild
    msb.solution  = SOLUTION_FILE
  end
end

namespace :mono do

  SOLUTION_FILE = "SpeedyMailer.Mono.sln"
  OUTPUT_FOLDER = "../Out/Drone"

  desc "clean the solution"
  xbuild :clean do |msb|
    msb.targets :Clean
    msb.solution  = SOLUTION_FILE
  end

  desc "Build the solution"
  xbuild :build => :clean do |msb|
    msb.properties :configurations => :Release,:OutputPath => OUTPUT_FOLDER
    msb.targets :Rebuild
    msb.solution  = SOLUTION_FILE
  end
end



