import boto
from boto.s3.key import Key
import os
import datetime

def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M")

def upload_daily(fn):

	s3 = boto.connect_s3()
	b = s3.get_bucket('dyn-builds-data')

	k = Key(b)
	k.key = os.path.basename(fn)
	k.key = os.path.basename( 'daily/DynamoDailyInstall' + date_string() + '.exe')
	k.set_contents_from_filename(fn)


