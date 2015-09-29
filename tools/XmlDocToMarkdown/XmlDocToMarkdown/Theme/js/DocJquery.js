$(document).ready(function(){
		$("td:contains('TypeParam :')").css("border-top","1px #939598 solid");
		$("td:contains('TypeParam :')").next().css("border-top","1px #939598 solid");		
		$(window).scroll(function() {
			var $myDiv = $('#menuNavigation');
			var st = $(this).scrollTop();
			if($(window).scrollTop() + $(window).height() < $(document).height()) {
			var pageHeight = $(window).scrollTop() + $(window).height();
			var docHeight = $(document).height();
			if(docHeight - pageHeight >= 0){				
			    $myDiv.height(pageHeight);
			}
		}			
		}).scroll();
	}
);