
#include "Tonuino_RFID.h"

static Tonuino_RFID_Reader tonuinoRFID;

class Tonuino_RFID_Tool_Core
{
	public:
	static char readSerialString[20];
	
	void transmitCardData(nfcTagStruct nfcTag);
	void transmitCardRemoval();
	void listen();
	
	private:
	void transmitTrigger(bool startTrigger);
	void writeCard(nfcTagStruct nfcTag);
	void handleCommand();
};

 