
#include "Tonuino_Potentiometer.h"

int TonuinoPotentiometer::hysterese = 0;  // Volumepoti Hysterese (usually = 2)
int TonuinoPotentiometer::lastValue = CONFIG_VolumeInit;  // old Poti Value, Volume
int TonuinoPotentiometer::pin = 0;

void TonuinoPotentiometer::setup(int potiPin)
{
	pin = potiPin;
	pinMode(pin, INPUT_PULLUP);
}

int TonuinoPotentiometer::read()
{
    int value = analogRead(pin);
    value = map(value, 0, 1024, CONFIG_VolumeMin, CONFIG_VolumeMax);
    // Compare current value with last incl. hysterese
    if ((value > lastValue + hysterese && value <= CONFIG_VolumeMax)
     || (value < lastValue - hysterese && value >= CONFIG_VolumeMin))  
    {
        Serial.print("Potentiometer Value: ");
        Serial.println(value);       
        lastValue = value;
    }
}



