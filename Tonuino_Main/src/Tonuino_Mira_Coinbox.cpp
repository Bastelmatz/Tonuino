
#include "Tonuino_Defines_Tools.h"

#ifdef TONUINO_MIRA_COINBOX

#include "Tonuino_Mira_Coinbox.h"
#include "shared/Tonuino_Config.h"
#include "shared/Tonuino_MainController.h"

TonuinoConfig getConfigMira()
{
	TonuinoConfig config;
	
	config.PINS.StopLED = 11; // 6
	config.PINS.Shutdown = 12; // 7

	config.PINS.Encoder_CLK = 6; // 5
	config.PINS.Encoder_DT = 7;  // 8

	config.HW.CardReader = false;
	config.HW.UltraSonic = true;
	config.HW.RotaryEncoder = true;
	
	config.SW.StartMusicDS.folder = 10;
	config.SW.StartMusicDS.mode = 1;
	
	return config;
}

void setupMira()
{
	TonuinoConfig config = getConfigMira();
	
	setupTonuino(config);
}

void loopMira()
{
	loopTonuino();
}

#endif
