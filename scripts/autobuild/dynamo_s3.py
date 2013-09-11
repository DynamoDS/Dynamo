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
	
	print "Upload progress: %.2f mb / %.2f mb ( %.2f percent )" % (mb(bytes_so_far), mb(total_bytes), float(bytes_so_far)/total_bytes)

def upload_daily(fn):

	s3 = boto.connect_s3()
	b = s3.get_bucket('dyn-builds-data')

	k = Key(b)
	k.key = os.path.basename(fn)
	k.key = os.path.basename( 'daily/DynamoDailyInstall' + date_string() + '.exe')
	k.set_contents_from_filename(fn, cb = report_progress, num_cb = 40)


