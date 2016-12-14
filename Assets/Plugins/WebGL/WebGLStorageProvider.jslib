var Plugin = {
	WebGl_Storage_Initialize: function()
	{
		this._counter = 0;
		this._activeRequests={};
		this.stringToArrayBuffer = function(str)
			{
				var uintArray = [];
				for (var i=0; i<str.length; i++) {
					var v = str.charCodeAt(i);
					uintArray.push(v);
				}
				
				return new Uint8Array(uintArray);
			};
		this.ua2hex = function(ua)
		{
			var h = '';
			for (var i = 0; i < ua.length; i++) {
				h += ua[i].toString(16);
			}
			return h;
		};
	},
	WebGl_Storage_IsInitialized: function()
	{
		return (typeof this._activeRequests) !== "undefined"
	},
	LoadRemoteFile: function(path)
	{
		var url = Pointer_stringify(path);
		var request = new XMLHttpRequest();
		request.open("GET",url,false);
		request.overrideMimeType('text/plain; charset=x-user-defined');
		
		console.log("Downloading: \""+url+"\"");
		
		var obj ={error: null};
		obj.url = url;
		try
		{
			request.send(null);
			obj.data = this.stringToArrayBuffer(request.responseText);
			console.log("Finished downloading: \""+url+"\"");
		}
		catch(e)
		{
			console.log("Error downloading \""+url+"\"");
			console.log(e);
			obj.error = e;
		}		
		
		var id = this._counter;
		this._counter++;
		this._activeRequests[id.toString()] = obj;
		return id;
	},
	HasErrors: function(id)
	{
		var obj = this._activeRequests[id.toString()];
		if(obj.error == null)
			return null;
		
		var buffer = _malloc(lengthBytesUTF8(obj.error) + 1);
        writeStringToMemory(obj.error, buffer);
        return buffer;
	},
	WebGl_Storage_IsEndOfFile: function(id, index)
	{
		var obj = this._activeRequests[id.toString()];
		if(obj == null || obj.error!=null)
			return true;
		
		return obj.data.length <= index;
	},
	WebGl_Storage_ReadBytes: function(id, buffer, index, toRead)
	{
		var obj = this._activeRequests[id.toString()];
		if(obj.error != null)
			return -1;
		
		var max = Math.min(toRead,obj.data.length-index);
		for(i = 0; i<max; i++)
		{	
			var p = buffer+i*HEAPU8.BYTES_PER_ELEMENT;
			setValue(p,obj.data[index+i],'i8');
		}
		
		return max;
	}
};

mergeInto(LibraryManager.library, Plugin);