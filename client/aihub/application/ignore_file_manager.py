import os

from constants import DEFAULT_IGNORE_LIST
from log import logger as aihub_logger

class IgnoreFileManager(object):
    """
    Manages .ai-ignore file in current directory
    """

    IGNORE_FILE_PATH = os.path.join(os.getcwd() + "/.ai-ignore")

    @classmethod
    def init(self):
        if os.path.isfile(self.IGNORE_FILE_PATH):
            aihub_logger.debug(f"AIHUB ignore file is already at {self.IGNORE_FILE_PATH}")
            return
        
        aihub_logger.debug(f"Setting default ignore list in file at {self.IGNORE_FILE_PATH}")

        # This creates file with default ones
        with open(self.IGNORE_FILE_PATH, "w") as ignore_file:
            ignore_file.write(DEFAULT_IGNORE_LIST)

    def slash_prefix_trim(path):
        """
        Remove '/' if it is 1st character in path.
        PurePath wants path as absolute path (for match)
        """
        if path.startswith('/'):
            return path[1:]
        return path

    @classmethod
    def get_lists(self, file_path=None):
        if file_path != None:
            ignore_file_path = file_path
        else:
            ignore_file_path = self.IGNORE_FILE_PATH

        # Safety if somebody gave not a file
        if not os.path.isfile(ignore_file_path):
            return([], [])

        ignore_list = []
        whitelist = []

        with open(ignore_file_path, "r") as ignore_file:
            for line in ignore_file:
                line = line.strip() # Remove all spaces etc.

                if not line or line.startswith('#'):
                    continue

                if line.startswith('!'):
                    line = line[1:]
                    whitelist.append(self.slash_prefix_trim(line))
                    continue

                # To allow escaping file names that start with !, # or \
                # remove the escaping \
                if line.startswith('\\'):
                    line = line[1:]

                ignore_list.append(self.slash_prefix_trim(line))

        aihub_logger.debug(f"IGNORE_LIST: {ignore_list}")
        aihub_logger.debug(f"WHITELIST: {whitelist}")
        return (ignore_list, whitelist)

            