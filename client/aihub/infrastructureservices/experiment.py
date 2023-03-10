import sys
from aihub.infrastructureservices.base import HttpClient
from aihub.infrastructureservices.files import get_files_in_current_directory, human_friendly_size
from log import logger as aihub_logger

class ExperimentClient(HttpClient):
    """
    Client to interact with Experiment API
    """
    MAX_UPLOAD_SIZE = 2_147_483_647 # Max int size in ASP.NET C#

    def __init__(self):
        self.url_init = "/experiments/init"
        self.url_stop = "/experiments/stop"
        self.url_upload = "/experiments/upload"
        super().__init__() # Call __init__() on HttpClient
    
    def init(self, experiment_config):
        """
        Make folder in server with project.
        """
        aihub_logger.debug("ExperimentClient - initialize")

        response = self.request('POST', self.url_init, json=experiment_config.to_dict())
        return response.json()['result']

    def upload(self, upload_config):
        """
        Upload files to a server
        """
        try:
            upload_files, total_file_size = get_files_in_current_directory(file_type='files')
        except OSError:
            sys.exit("Directory contains too many files to upload. If you have data files in the current directory, "
                     "please pack them and send to github and download in python scipt and remove them from here.\n")

        if total_file_size > self.MAX_UPLOAD_SIZE:
            sys.exit(("Code size too large to sync, please keep it under %s.\n"
            "If you have data files in the current directory, "
            "please pack them and send to github and download in python scipt and remove them from here.\n") % (human_friendly_size(self.MAX_UPLOAD_SIZE)))

        aihub_logger.info("Uploading files. Total upload size: %s",
                          human_friendly_size(total_file_size))
        aihub_logger.info("Uploading: %s files",
                           len(upload_files))

        files_only = [x[1] for x in upload_files]
        file_name_only = [x[0] for x in files_only]
        aihub_logger.info(f"List of files being uploaded: {file_name_only}")

        # Creating payload
        payload = upload_config.to_dict() # instance type, experiment_name etc; more in run.py or Module class in models
        aihub_logger.debug("payload: ")
        aihub_logger.debug(payload)
        
        aihub_logger.debug("upload_files: ")
        aihub_logger.debug(upload_files)

        aihub_logger.info("Uploading...")
        response = self.request("POST",
                                self.url_upload,
                                files=upload_files,
                                data=payload,
                                timeout=3600)            
            
        return response.json()['result']

    def stop(self, module):
        # Creating payload
        payload = module.to_dict()
        aihub_logger.debug(payload)

        try:
            aihub_logger.info("Stopping current run.")
            response = self.request("POST",
                                    self.url_stop,
                                    json=payload,
                                    timeout=3600)            
        finally:
            aihub_logger.info("Run stop request done.")

        return response.json()['result']
