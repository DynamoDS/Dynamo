import boto3
import os
import datetime
from decimal import *

def date_string():
    return datetime.datetime.now().strftime("%Y%m%dT%H%M")

def mb(num_bytes):
    return float(num_bytes) / 1000000

def report_progress(bytes_so_far, total_bytes):
    print("Upload progress: %.2f mb / %.2f mb ( %.2f percent )" % (mb(bytes_so_far), mb(total_bytes), 100 * float(bytes_so_far) / total_bytes))

def upload_installer(fn, prefix, include_date, is_dev_build, extension, date_string):
    s3 = boto3.client('s3')

    if extension == '.exe' or extension == '.zip':
        bucket_name = 'dyn-builds-dev' if is_dev_build else 'dyn-builds-data'
    elif extension == '.sig':
        bucket_name = 'dyn-builds-dev-sig' if is_dev_build else 'dyn-builds-data-sig'

    key = 'daily/' + prefix + '_' + date_string + extension if include_date else 'daily/' + prefix + extension

    s3.upload_file(
        Filename=fn,
        Bucket=bucket_name,
        Key=os.path.basename(key),
        Callback=lambda bytes_transferred: report_progress(bytes_transferred, os.path.getsize(fn))
    )
