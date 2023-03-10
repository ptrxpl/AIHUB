import click
import client
from aihub.cli.experiment import init, upload, stop
from aihub.cli.run import run
from aihub.cli.auth import login, logout
from log import configure_logger

@click.group()
@click.option('-h', '--host', default='https://localhost:7242', help='AIHUB server endpoint')
@click.option('-q', '--quiet', count=True, help='AIHUB will be quiet during operations (logs only in file)')
def cli(host, quiet):
    """
    AIHUB CLI
    """
    client.aihub_host = host
    configure_logger(quiet)

def add_commands(cli):
    """
    Register commands here.
    """
    cli.add_command(login)
    cli.add_command(init)
    cli.add_command(upload)
    cli.add_command(run)
    cli.add_command(stop)
    cli.add_command(logout)    

if __name__ == "__main__":
    add_commands(cli)
    cli()