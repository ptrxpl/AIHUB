import click
from aihub.application.auth_file_manager import AuthFileManager
from aihub.application.experiment_file_manager import ExperimentFileManager
from aihub.infrastructureservices.streamcmd import StreamCmdClient
from log import logger as aihub_logger

@click.command()
@click.argument('file', nargs=1)
@click.argument('how_much_vram', nargs=1)
@click.option('--run_again', is_flag=True, default=False)
def run(file, how_much_vram, run_again):
    """
    Runs given filename, depending if VRAM is avalaible. You can also set --run-again flag.
    """
    AuthFileManager.exists_auth()

    experiment_config = ExperimentFileManager.get_config()
    project_name = experiment_config['name']

    try:
        auth_settings = AuthFileManager.get_auth()
        access_token = auth_settings['access_token']

        signalr_client = StreamCmdClient(access_token)
        signalr_client.run(project_name, file, how_much_vram, run_again)
    except Exception as e:
        aihub_logger.error("Error: %s", e)