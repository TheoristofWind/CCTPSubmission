import createTrainingData
import carball
import json
import numpy as np

import os
from os import listdir
from os.path import isfile, join

class CustomJSONizer(json.JSONEncoder):
    def default(self, obj):
        return super().encode(bool(obj)) \
            if isinstance(obj, np.bool_) \
            else super().default(obj)

path = './replayfiles'
outPath = './outputfiles/'

checkFolder = os.path.isdir(outPath)

if not checkFolder:
    os.makedirs(outPath)

directories = [d for d in listdir(path) if not isfile(join(path, d))]
startingNumber = 925
outNumber = 0
outName = '0'

for dir in directories:
    files = listdir(path + '/' + dir)
    for f in files:
        if startingNumber > outNumber:
            outNumber += 1
            outName = str(outNumber)
        else:
            _json = carball.decompile_replay(path + '/' + dir + '/' + f)
            with open("out.json", 'w') as outfile:
                json.dump(_json, outfile)
            createTrainingData.createAndSaveReplayTrainingDataFromJSON("out.json", outputFileName = "exampleTrainingData.pbz2")
            gameData = createTrainingData.loadSavedTrainingData("exampleTrainingData.pbz2")
            with open(outPath + outName + '.json', 'w') as outfile:
                json.dump(gameData, outfile, cls=CustomJSONizer)
            outNumber += 1
            outName = str(outNumber)