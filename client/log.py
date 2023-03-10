import logging
import os

logger = logging.getLogger('aihub')

def configure_logger(quiet):
    # https://docs.python.org/3/howto/logging-cookbook.html
    # .debug - logs all time to file
    # .info - logs to console

    LOGS_PATH = os.getcwd() + "/logs"

    if not os.path.exists(LOGS_PATH):
        os.makedirs(LOGS_PATH)      

    # Set up logging to file 
    logging.basicConfig(level=logging.DEBUG,
                    format = '%(asctime)s %(name)s %(levelname)-6s [%(filename)s:%(lineno)d] %(message)s',
                    datefmt = '%d-%m-%Y %H:%M:%S',
                    filename = LOGS_PATH + '/logs.log',
                    filemode = 'a',
                    encoding= 'utf-8',)

    # Define a Handler which writes INFO messages or higher to the sys.stderr
    console = logging.StreamHandler()
    console.setLevel(logging.INFO) 
    # Set a format which is simpler for console use
    formatter = logging.Formatter('%(message)s')
    # tell the handler to use this format
    console.setFormatter(formatter)

    # add the handler to the root logger
    if quiet == False:
        logger.addHandler(console)

    logger.debug("User has just started the app. Logger initialized.")

