import click
import uuid
from aihub.application.auth_file_manager import AuthFileManager
from aihub.application.experiment_file_manager import ExperimentFileManager
from aihub.application.ignore_file_manager import IgnoreFileManager
from aihub.domain.experiment_config import ExperimentConfig
from aihub.domain.upload_config import UploadConfig
from aihub.infrastructureservices.experiment import ExperimentClient
from log import logger as aihub_logger

@click.command()
@click.argument('project_name', nargs=1)
def init(project_name):
    """
    Initialize new project.
    After this you can run other commands like "upload" or "run".
    """
    AuthFileManager.exists_auth()

    # Check if .ai-exp already exists, if yes - show error and sys.exit()
    ExperimentFileManager.exist_config()

    # Get project name from user
    name = click.prompt('Press ENTER to use project name "%s" or enter a different name' % project_name, default=project_name, show_default=False)
    project_name = name.strip() or project_name

    # Create .ai-exp JSON with proj name
    experiment_config = ExperimentConfig(name=project_name + "_" + str(uuid.uuid4()))
    ExperimentFileManager.set_config(experiment_config)

    # Create .ai-ignore
    IgnoreFileManager.init() 

    # Sending request to create dictionary on server with folder
    try:
        init_response = ExperimentClient().init(experiment_config)
        aihub_logger.info(f"[INFO] {init_response}")
    except Exception as e:
        aihub_logger.error("[ERROR] %s", e)  
   
@click.command()
def upload():
    """
    AIHUB will upload contents of the current directory to server.
    """
    AuthFileManager.exists_auth()

    experiment_config = ExperimentFileManager.get_config()
    upload_config = UploadConfig(name=experiment_config['name'])

    try:
        upload_response = ExperimentClient().upload(upload_config) 
        aihub_logger.info(f"[INFO] {upload_response}")       
    except Exception as e:
        aihub_logger.error("[ERROR] %s", e)   

@click.command()
def stop():
    """
    Stop current running process.
    """
    AuthFileManager.exists_auth()

    experiment_config = ExperimentFileManager.get_config()
    upload_config = UploadConfig(name=experiment_config['name']) 

    try:
        stop_response = ExperimentClient().stop(upload_config)
        aihub_logger.info(f"[INFO] {stop_response}")      
    except Exception as e:
        aihub_logger.error("[ERROR] %s", e)  