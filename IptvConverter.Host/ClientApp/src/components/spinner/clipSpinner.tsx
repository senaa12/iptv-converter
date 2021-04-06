import * as React from 'react';

import './clipSpinner.scss';

export interface ClipSpinnerProps {
    size?: number;
	color?: string;
}

export default ({ color, size }: ClipSpinnerProps) => {
    return (
		<div
			className={'clip-spinner'}
			style={{
				borderLeftColor: color ?? 'black',
				borderTopColor: color ?? 'black',
				borderRightColor: color ?? 'black',
				borderBottomColor: 'transparent',
				width: size,
				height: size,
			}}
		/>
	);
};
