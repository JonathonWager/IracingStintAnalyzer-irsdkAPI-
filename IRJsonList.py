#!python3

import winsound
import irsdk
import json
import time
import os
import keyboard
from datetime import datetime
import math
import sys


# this is our State class, with some helpful variables
class State:
    ir_connected = False
    last_car_setup_tick = -1
    
    last_track_temp_tick = False
    last_air_temp_tick = False
    last_lap_time_tick = -1

    retrieved_track_name = -1

class sessionDetails:
    sessionDetails = {}
    track_name = ""
    car_name = ""
    starting_track_temp = 0
    starting_air_temp = 0
    start_time = 0
class stintDetails:
    stintDetails = {}
    track_temp = 0
    air_temp = 0
    starting_fuel = 0
    track_name = ""
    car_name = ""
    session_time = ""
    lap_count = 0
    real_world_time = 0
    fastest_lap_time = 999999

class Dictonary:
    currentLap = {}
    stintLaps = []
    LapNumber = 0
    lapTime = 0
    FuelUsage = 0
    FuelRemaining = 0
    lapsRemaining = 0
    top_speed = 0

class lapTimes:
    lap_count = 0
    prev_fuel = 0

class stintType:
    raceStint = False
    qualStint = False 
    stintCount = 0
    stint_in_progress = False   
    session_started = False
# here we check if we are connected to iracing
# so we can retrieve some data
def check_iracing():
    if state.ir_connected and not (ir.is_initialized and ir.is_connected):
        state.ir_connected = False
        # don't forget to reset your State variables
        state.last_car_setup_tick = -1
        # we are shutting down ir library (clearing all internal variables)
        ir.shutdown()
        print('irsdk disconnected')
        sys.exit()
    elif not state.ir_connected and ir.startup() and ir.is_initialized and ir.is_connected:
        state.ir_connected = True
        print('irsdk connected')
        playStartSound()
      

def if_stint_started():
    if stintType.stint_in_progress == True:
        return True
    else:
        return False    

def getstintType():
    if stintType.raceStint == True:
        return "Race"

    if stintType.qualStint == True:
        return "Qual"

def getCurrentTime():
    #return datetime.datetime.now().time()
    return datetime.now().strftime("%H:%M")

def endStint():
    stintType.raceStint = False
    stintType.qualStint = False
    stintType.stint_in_progress = False 
    stintDetails.fastest_lap_time = 999999    
    lapTimes.lap_count = 0
    lapTimes.prev_fuel = 0
    State.last_lap_time_tick = -1
    Dictonary.stintLaps = []

def calculate_fuel():
    lap_fuel = ir['FuelLevel']  
    if lap_fuel: 
        Dictonary.FuelRemaining = lap_fuel  
        if(lapTimes.lap_count == 1):
            Dictonary.FuelUsage =  stintDetails.starting_fuel - lap_fuel
            lapTimes.prev_fuel = lap_fuel
            
            
        else:
            Dictonary.FuelUsage =  lapTimes.prev_fuel - lap_fuel
            lapTimes.prev_fuel = lap_fuel
        laps_remaining = math.floor(lapTimes.prev_fuel / Dictonary.FuelUsage)
     
        Dictonary.lapsRemaining = laps_remaining

def get_car():
    drivers = ir['DriverInfo']['Drivers']
    if drivers:
        for driver in drivers:
            #
            #Dont Forget
            #
            if driver['UserName'] == "Jonathon Wager":
               return driver['CarScreenNameShort']
def get_track():
    track = ir['WeekendInfo']['TrackDisplayShortName']
    if track:
        return track
def get_air_temp():
    air_temp = ir['AirTemp']
    if air_temp:    
        return air_temp
def get_track_temp():
    track_temp = ir['TrackTemp']
    if track_temp:
       return track_temp
def get_session_time():
    session_time = ir['SessionTimeOfDay']
    if session_time:
        return session_time
def get_fuel_level():
    lap_fuel = ir['FuelLevel']  
    if lap_fuel: 
        return lap_fuel
def get_session_details(dir_string):

    sessionDetails.car_name = get_car()
    sessionDetails.track_name = get_track()
    sessionDetails.starting_air_temp = get_air_temp()
    sessionDetails.starting_track_temp = get_track_temp()
    sessionDetails.start_time = get_session_time()
   
    sessionDetails.sessionDetails= {
        "TrackName"  : sessionDetails.track_name,
        "CarName"  : sessionDetails.car_name,
        "SessionStartTime" : sessionDetails.start_time,
        "StartTrackTemp" : sessionDetails.starting_track_temp,
        "StartAirTemp"  : sessionDetails.starting_air_temp,
    }
    
    with open("Sessions/"+ dir_string+"/"+ "SessionInfo" +".json", 'w') as outfile:
        json.dump(sessionDetails.sessionDetails, outfile ,indent=4)
def get_stint_details():  
   
    stintDetails.track_name = get_track()
    stintDetails.car_name = get_car()
    stintDetails.track_temp = get_track_temp()
    stintDetails.air_temp = get_air_temp()
    stintDetails.session_time = get_session_time()
    stintDetails.starting_fuel = get_fuel_level()
    stintDetails.real_world_time = getCurrentTime()  
  
    
def playRaceSound():
    winsound.PlaySound("audio/racestint.wav", winsound.SND_ASYNC | winsound.SND_ALIAS )
def playQualSound():
    winsound.PlaySound("audio/qualstint.wav", winsound.SND_ASYNC | winsound.SND_ALIAS )

def playEndSound():
    winsound.PlaySound("audio/stintend.wav", winsound.SND_ASYNC | winsound.SND_ALIAS )

def playStartSound():
    winsound.PlaySound("audio/itstart.wav", winsound.SND_ASYNC | winsound.SND_ALIAS )

def write_stint_details(dir_string):
    if stintDetails.fastest_lap_time == 999999:
        fastest_lap = 0
    else:
        fastest_lap = stintDetails.fastest_lap_time   
    stintDetails.stintDetails ={
        "TrackTemp" : stintDetails.track_temp,
        "AirTemp"  : stintDetails.air_temp,
        "StartingFuel" : stintDetails.starting_fuel,
        "TrackName"  : stintDetails.track_name,
        "CarName"  : stintDetails.car_name,
        "SessionTime" : stintDetails.session_time,
        "LapCount" : lapTimes.lap_count,
        "CurrentTime" : stintDetails.real_world_time,
        "FastestLap" : fastest_lap,
    }
    
    with open("Sessions/"+ dir_string+"/"+ getstintType() + str(stintType.stintCount) + "_StintInfo" +".json", 'w') as outfile:
        json.dump(stintDetails.stintDetails, outfile ,indent=4)

def loop(dir_string):
    
    ir.freeze_var_buffer_latest()
    if not if_stint_started():
        if keyboard.is_pressed('R'):
            print('Race Stint Started Press E to end stint')
            playRaceSound()
            stintType.raceStint = True
            stintType.stint_in_progress = True
            get_stint_details()
            
        if keyboard.is_pressed('Q'):
            print('Qual Stint Started Press E to end stint')
            playQualSound()
            stintType.qualStint = True
            stintType.stint_in_progress = True  
            get_stint_details()

    else:
        speed = ir['Speed']
        if speed:
            if speed > Dictonary.top_speed:
                Dictonary.top_speed = speed
        lap_time = ir['LapLastLapTime']  
        if lap_time:
            lap_time_tick = lap_time
            if lap_time_tick != state.last_lap_time_tick:
                state.last_lap_time_tick = lap_time_tick
                
                lapTimes.lap_count += 1
                Dictonary.lapTime = lap_time
                if lap_time > 0 and lap_time < stintDetails.fastest_lap_time:
                    stintDetails.fastest_lap_time = lap_time
                Dictonary.LapNumber = lapTimes.lap_count
                
                calculate_fuel()
                
                Dictonary.currentLap = {
                    "lapNumber": Dictonary.LapNumber,
                    "lapTIme": Dictonary.lapTime,
                    "fuelUsage": Dictonary.FuelUsage,
                    "fuelRemaining": Dictonary.FuelRemaining,
                    "lapRemaining": Dictonary.lapsRemaining,
                    "topSpeed" : Dictonary.top_speed
                }
                Dictonary.top_speed = 0

                Dictonary.stintLaps.append(Dictonary.currentLap)
                print("added lap")
        if keyboard.is_pressed('E'):
            print('Stint Ended Writing Data')
            playEndSound()
            stintType.stintCount += 1
            write_stint_details(dir_string)
            with open("Sessions/"+ dir_string+"/"+ getstintType() + str(stintType.stintCount) +".json", 'w') as outfile:
                json.dump(Dictonary.stintLaps, outfile ,indent=4)
            endStint()  


if __name__ == '__main__':
    # initializing ir and state
    ir = irsdk.IRSDK()
    state = State()
    now = datetime.now()
    print("Waiting For Iracing SDK")
    
    try:
        # infinite loop
        while True:
            # check if we are connected to iracing
            check_iracing()
            # if we are, then process data
            if state.ir_connected:
                if stintType.session_started == False:
                    stintType.session_started = True
                    
                    dir_string =  str(now).replace(" ", "-").replace(".", "-").replace(":", "-")
                    os.mkdir("Sessions/"+dir_string)
          
                    get_session_details(dir_string)

                loop(dir_string)

            # sleep for 1 second
            # maximum you can use is 1/60
            # cause iracing updates data with 60 fps
            time.sleep(1)
    except KeyboardInterrupt:
        # press ctrl+c to exit
        pass
    
    
     
