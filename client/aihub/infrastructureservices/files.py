import os
from pathlib2 import PurePath
from aihub.application.ignore_file_manager import IgnoreFileManager
from log import logger as aihub_logger

def get_files_in_current_directory(file_type):
    """
    Gets the list of files in the current directory and subdirectories.
    Respects .aihubignore file if present
    """
    local_files = []
    total_file_size = 0

    ignore_list, whitelist = IgnoreFileManager.get_lists()

    file_paths = get_file_paths_respecting_ignore_file(ignore_list, whitelist)

    for file_path in file_paths:
        local_files.append((file_type, (path_separator_unix(file_path), open(file_path, 'rb'), 'text/plain')))
        total_file_size += os.path.getsize(file_path)

    return (local_files, total_file_size)

def get_file_paths_respecting_ignore_file(ignore_list=None, whitelist=None):
    """
    In .ai-ignore there are ignored list and whitelist for some files.
    This method returns file paths in current directory and its subdirectories, respecting rules given in .ai-ignore file.
    """
    unignored_files = []

    if ignore_list is None:
        ignore_list = []

    if whitelist is None:
        whitelist = []

    for root, dirs, files in os.walk("."):
        if ignore_path(path_separator_unix(root), ignore_list, whitelist):
            # Clear directories to avoid going inside of them (because it is in ignore_list)
            # Continue to next iteration of os.walk, to ignore current directory.
            # If some file is in whitelist, but in the same time it is inside of ingore_list, it will be NOT whitelisted (like in .gitignore).
            dirs[:] = []
            continue

        for file_name in files:
            file_path = path_separator_unix(os.path.join(root, file_name))
            if ignore_path(file_path, ignore_list, whitelist):
                continue
            
            unignored_files.append(os.path.join(root, file_name)) # There operate with system paths

    aihub_logger.debug(f"unignored_files: {unignored_files}")
    return unignored_files

def ignore_path(path, ignore_list=None, whitelist=None):
    """
    Returns boolean that indicate if path should be ignored in given ignore_list and not ignored in whitelist
    """
    if ignore_list is None:
        return True
    
    should_ignore = is_path_in_list(path, ignore_list)
    if whitelist is None:
        return should_ignore

    return should_ignore and not is_path_in_list(path, whitelist)

def path_separator_unix(path):
    # Because ignore_file or whitelist has unix style inside.
    # Windows: "\"
    # Unix: "/"
    # If path separator is Windows style, change it to Unix style.

    if os.path.sep != "/": 
        return path.replace(os.path.sep, '/')

    return path    

def is_path_in_list(path, list):
    """
    Check if path is in given list of paths
    """

    for path_in_list in list:
        try:
            if PurePath(path).match(path_in_list):
                return True
        except:
            pass

    return False

def human_friendly_size(num, suffix='B'):
    """
    Source: 
    https://stackoverflow.com/questions/1094841/get-human-readable-version-of-file-size
    """
    for unit in ['', 'Ki', 'Mi', 'Gi', 'Ti', 'Pi', 'Ei', 'Zi']:
        if abs(num) < 1024.0:
            return "%3.1f%s%s" % (num, unit, suffix)
        num /= 1024.0
    return "%.1f%s%s" % (num, 'Yi', suffix)