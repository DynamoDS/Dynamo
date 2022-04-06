import boto
from boto.s3.key import Key
import os
import datetime
from decimal import *

def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M")

def mb(num_bytes):

	return float(num_bytes) / 1000000

def report_progress( bytes_so_far, total_bytes ):
	
	print("Upload progress: %.2f mb / %.2f mb ( %.2f percent )" % (mb(bytes_so_far), mb(total_bytes), 100 * float(bytes_so_far)/total_bytes))

def upload_installer(fn, prefix, include_date, is_dev_build, extension, date_string):

	s3 = boto.connect_s3()

	if extension == '.exe' or extension == '.zip':
		if is_dev_build:
			b = s3.get_bucket('dyn-builds-dev')
		else:
			b = s3.get_bucket('dyn-builds-data')
	elif extension == '.sig':
		if is_dev_build:
			b = s3.get_bucket('dyn-builds-dev-sig')
		else:
			b = s3.get_bucket('dyn-builds-data-sig')

	k = Key(b)
	
	key = 'daily/' + prefix + '_' + date_string + extension if include_date else 'daily/' + prefix + extension
	
	k.key = os.path.basename( key )

	k.set_contents_from_filename(fn, cb = report_progress, num_cb = 40)



