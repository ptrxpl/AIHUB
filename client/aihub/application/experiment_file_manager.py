import json
import os
import sys
import click
from aihub.domain.experiment_config import ExperimentConfig
from log import logger as aihub_logger

class ExperimentFileManager(object):
    """
    Manages .ai-exp file in the current directory
    """

    CONFIG_FILE_PATH = os.path.join(os.getcwd() + "/.ai-exp")

    @classmethod
    def set_config(self, experiment_config):
        with open(self.CONFIG_FILE_PATH, "w") as config_file:
            config_file.write(json.dumps(experiment_config.to_dict()))

    @classmethod
    def get_config(self):
        if not os.path.isfile(self.CONFIG_FILE_PATH):
            raise Exception("[ERROR] Missing .ai-exp file, run init first")

        with open(self.CONFIG_FILE_PATH, "r") as config_file:
            experiment_config = json.loads(config_file.read())

        return ExperimentConfig.from_dict(experiment_config)

    @classmethod
    def exist_config(self):
        is_init_already = os.path.isfile(self.CONFIG_FILE_PATH)
        if is_init_already is True:
            click.echo(click.style('[INFO] There is already initialized AIHUB project in that folder! Init skipped. You can delete manually .ai-exp file.', fg='yellow'))
            sys.exit()